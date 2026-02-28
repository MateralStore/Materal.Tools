import { useState, useEffect, useRef, useCallback } from 'react'
import { SignalRClient, ConnectionState, Message, createMessage } from './SignalRClient'

const STORAGE_KEYS = {
  hubUrl: 'signalr_hubUrl',
  listeners: 'signalr_listeners',
  methods: 'signalr_methods',
} as const

export interface MethodItem {
  id: string
  name: string
  params: string
}

function getStoredValue<T>(key: string, defaultValue: T): T {
  try {
    const stored = localStorage.getItem(key)
    return stored ? JSON.parse(stored) : defaultValue
  } catch {
    return defaultValue
  }
}

function App() {
  const [hubUrl, setHubUrl] = useState(() => getStoredValue(STORAGE_KEYS.hubUrl, 'http://localhost:5000/chat'))
  const [connectionState, setConnectionState] = useState<ConnectionState>('Disconnected')
  const [messages, setMessages] = useState<Message[]>([])
  const [error, setError] = useState<string | null>(null)
  const [listenMethodName, setListenMethodName] = useState('MessageReceived')
  const [listeners, setListeners] = useState<string[]>(() => getStoredValue(STORAGE_KEYS.listeners, []))
  const [methods, setMethods] = useState<MethodItem[]>(() => getStoredValue(STORAGE_KEYS.methods, [{ id: '1', name: 'SendMessage', params: '' }]))
  const [autoScroll, setAutoScroll] = useState(true)

  const clientRef = useRef<SignalRClient>(new SignalRClient())
  const messagesEndRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    localStorage.setItem(STORAGE_KEYS.hubUrl, JSON.stringify(hubUrl))
  }, [hubUrl])

  useEffect(() => {
    localStorage.setItem(STORAGE_KEYS.listeners, JSON.stringify(listeners))
  }, [listeners])

  useEffect(() => {
    localStorage.setItem(STORAGE_KEYS.methods, JSON.stringify(methods))
  }, [methods])

  // 连接成功后恢复监听器
  useEffect(() => {
    if (connectionState === 'Connected' && listeners.length > 0) {
      const client = clientRef.current
      // 先清理旧的监听器，避免重复
      listeners.forEach(methodName => client.off(methodName))
      // 重新注册监听器
      listeners.forEach(methodName => {
        client.on(methodName, (...args) => {
          const content = `${methodName}: ${args.map(arg => JSON.stringify(arg)).join(', ')}`
          setMessages((prev) => [...prev, createMessage('receive', content)])
        })
      })
    }
  }, [connectionState, listeners])

  useEffect(() => {
    const client = clientRef.current

    client.onStateChanged = (state) => {
      setConnectionState(state)
    }

    client.onMessageReceived = (message) => {
      setMessages((prev) => [...prev, createMessage('receive', message)])
    }

    return () => {
      client.disconnect()
    }
  }, [])

  useEffect(() => {
    if (autoScroll) {
      messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
    }
  }, [messages, autoScroll])

  const handleConnect = useCallback(async () => {
    setError(null)
    try {
      await clientRef.current.connect(hubUrl)
    } catch (err) {
      setError(err instanceof Error ? err.message : '连接失败')
    }
  }, [hubUrl])

  const handleDisconnect = useCallback(async () => {
    setError(null)
    // 只清理 SignalR 连接上的监听器，不清空状态
    listeners.forEach(name => clientRef.current.off(name))
    await clientRef.current.disconnect()
  }, [listeners])

  const handleAddListener = useCallback(() => {
    if (!listenMethodName.trim()) return
    if (listeners.includes(listenMethodName)) {
      setError('该监听已存在')
      return
    }
    if (connectionState !== 'Connected') {
      setError('请先连接')
      return
    }
    try {
      clientRef.current.on(listenMethodName, (...args) => {
        const content = `${listenMethodName}: ${args.map(arg => JSON.stringify(arg)).join(', ')}`
        setMessages((prev) => [...prev, createMessage('receive', content)])
      })
      setListeners(prev => [...prev, listenMethodName])
      setListenMethodName('')
      setError(null)
    } catch (err) {
      setError(err instanceof Error ? err.message : '注册监听失败')
    }
  }, [listenMethodName, listeners, connectionState])

  const handleRemoveListener = useCallback((methodName: string) => {
    clientRef.current.off(methodName)
    setListeners(prev => prev.filter(m => m !== methodName))
  }, [])

  const handleAddMethod = useCallback(() => {
    const newMethod: MethodItem = {
      id: Date.now().toString(),
      name: '',
      params: '',
    }
    setMethods(prev => [...prev, newMethod])
  }, [])

  const handleRemoveMethod = useCallback((id: string) => {
    setMethods(prev => prev.filter(m => m.id !== id))
  }, [])

  const handleUpdateMethod = useCallback((id: string, field: 'name' | 'params', value: string) => {
    setMethods(prev => prev.map(m => m.id === id ? { ...m, [field]: value } : m))
  }, [])

  const handleSend = useCallback(async (method: MethodItem) => {
    setError(null)
    try {
      let args: string[] = []
      let content: string
      if (method.params.trim()) {
        // 解析参数，支持逗号分隔多个参数
        args = method.params.split(',').map(arg => arg.trim())
        content = `${method.name}(${args.join(', ')})`
      } else {
        content = `${method.name}()`
      }
      const result = await clientRef.current.send<string>(method.name, ...args)
      setMessages((prev) => [...prev, createMessage('send', content)])
      if (result !== null && result !== undefined) {
        setMessages((prev) => [...prev, createMessage('receive', `返回结果: ${result}`)])
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : '发送失败')
    }
  }, [])

  const getStateText = (state: ConnectionState): string => {
    switch (state) {
      case 'Connected':
        return '已连接'
      case 'Connecting':
        return '连接中'
      case 'Reconnecting':
        return '重连中'
      case 'Disconnected':
        return '已断开'
      default:
        return '未知'
    }
  }

  return (
    <div className="container">
      <h1>
        SignalR 客户端
        <span
          className={`status-indicator ${connectionState === 'Connected' ? 'status-connected' : 'status-disconnected'}`}
          title={getStateText(connectionState)}
        />
      </h1>

      <div className="main-layout">
        <div className="left-panel">
          <div className="connection-panel">
            <div className="form-group">
              <label>Hub 地址:</label>
              <div className="input-with-button">
                <input
                  type="text"
                  value={hubUrl}
                  onChange={(e) => setHubUrl(e.target.value)}
                  placeholder="http://localhost:5000/chat"
                />
                {connectionState === 'Connected' ? (
                  <button onClick={handleDisconnect} className="btn btn-disconnect btn-small">
                    断开
                  </button>
                ) : (
                  <button
                    onClick={handleConnect}
                    className="btn btn-connect btn-small"
                    disabled={connectionState === 'Connecting' || connectionState === 'Reconnecting'}
                  >
                    连接
                  </button>
                )}
              </div>
            </div>

            {error && <div className="error">{error}</div>}
          </div>

          <div className="listen-panel">
            <div className="form-group">
              <label>监听:</label>
              <div className="input-with-button">
                <input
                  type="text"
                  value={listenMethodName}
                  onChange={(e) => setListenMethodName(e.target.value)}
                  placeholder="方法名"
                  onKeyDown={(e) => e.key === 'Enter' && handleAddListener()}
                />
                <button
                  onClick={handleAddListener}
                  className="btn btn-small btn-connect"
                  disabled={connectionState !== 'Connected' || !listenMethodName.trim()}
                >
                  添加
                </button>
              </div>
            </div>
            {listeners.length > 0 && (
              <div className="listener-list">
                {listeners.map(name => (
                  <div key={name} className="listener-item">
                    <span>{name}</span>
                    <button
                      onClick={() => handleRemoveListener(name)}
                      className="btn-remove"
                    >
                      ×
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="send-panel">
            <div className="panel-header">
              <h3>方法调用</h3>
              <button className="btn btn-add" onClick={handleAddMethod}>+ 添加</button>
            </div>
            <div className="method-list">
              {methods.map(method => (
                <div key={method.id} className="method-item">
                  <button
                    onClick={() => handleRemoveMethod(method.id)}
                    className="btn-remove btn-remove-method"
                    title="删除"
                  >
                    ×
                  </button>
                  <div className="method-inputs">
                    <input
                      type="text"
                      value={method.name}
                      onChange={(e) => handleUpdateMethod(method.id, 'name', e.target.value)}
                      placeholder="方法名"
                    />
                    <input
                      type="text"
                      value={method.params}
                      onChange={(e) => handleUpdateMethod(method.id, 'params', e.target.value)}
                      placeholder="参数"
                    />
                  </div>
                  <button
                    onClick={() => handleSend(method)}
                    className="btn btn-send-small"
                    disabled={connectionState !== 'Connected' || !method.name.trim()}
                  >
                    发送
                  </button>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="right-panel">
          <div className="messages-panel">
            <div className="messages-header">
              <h3>消息列表 ({messages.length})</h3>
              <div className="header-actions">
                <label className="auto-scroll-label">
                  <input
                    type="checkbox"
                    checked={autoScroll}
                    onChange={(e) => setAutoScroll(e.target.checked)}
                  />
                  自动滚动
                </label>
                <button
                  className="btn btn-clear"
                  onClick={() => setMessages([])}
                  disabled={messages.length === 0}
                >
                  清空
                </button>
              </div>
            </div>
            <div className="messages-list">
              {messages.length === 0 && <div className="empty">暂无消息</div>}
              {messages.map((msg) => (
                <div key={msg.id} className={`message message-${msg.type}`}>
                  <span className="message-time">[{msg.time}]</span>
                  <span className="message-content">{msg.content}</span>
                </div>
              ))}
              <div ref={messagesEndRef} />
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default App
