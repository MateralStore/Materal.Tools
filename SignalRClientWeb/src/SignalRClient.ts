import * as signalR from '@microsoft/signalr'

export type ConnectionState = 'Disconnected' | 'Connecting' | 'Connected' | 'Reconnecting'

export interface Message {
  id: number
  type: 'send' | 'receive'
  content: string
  time: string
}

let messageId = 0

export class SignalRClient {
  private connection: signalR.HubConnection | null = null
  private _onMessageReceived: ((message: string) => void) | null = null
  private _onStateChanged: ((state: ConnectionState) => void) | null = null

  public get onMessageReceived() {
    return this._onMessageReceived
  }

  public set onMessageReceived(value: ((message: string) => void) | null) {
    this._onMessageReceived = value
  }

  public get onStateChanged() {
    return this._onStateChanged
  }

  public set onStateChanged(value: ((state: ConnectionState) => void) | null) {
    this._onStateChanged = value
  }

  public get connectionState(): ConnectionState {
    if (!this.connection) return 'Disconnected'
    switch (this.connection.state) {
      case signalR.HubConnectionState.Disconnected:
        return 'Disconnected'
      case signalR.HubConnectionState.Connecting:
        return 'Connecting'
      case signalR.HubConnectionState.Connected:
        return 'Connected'
      case signalR.HubConnectionState.Reconnecting:
        return 'Reconnecting'
      default:
        return 'Disconnected'
    }
  }

  public async connect(url: string): Promise<void> {
    if (this.connection) {
      await this.disconnect()
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .withAutomaticReconnect()
      .build()

    this.connection.onclose(() => {
      this._onStateChanged?.('Disconnected')
    })

    this.connection.onreconnecting(() => {
      this._onStateChanged?.('Reconnecting')
    })

    this.connection.onreconnected(() => {
      this._onStateChanged?.('Connected')
    })

    try {
      this._onStateChanged?.('Connecting')
      await this.connection.start()
      this._onStateChanged?.('Connected')
    } catch (err) {
      this._onStateChanged?.('Disconnected')
      throw err
    }
  }

  public async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      this._onStateChanged?.('Disconnected')
    }
  }

  public async send<T = unknown>(methodName: string, ...args: unknown[]): Promise<T | null> {
    if (!this.connection || this.connectionState !== 'Connected') {
      throw new Error('Not connected')
    }
    return await this.connection.invoke<T>(methodName, ...args)
  }

  public on(methodName: string, callback: (...args: unknown[]) => void): void {
    if (!this.connection) {
      throw new Error('Not connected')
    }
    this.connection.on(methodName, callback)
  }

  public off(methodName: string): void {
    if (!this.connection) return
    this.connection.off(methodName)
  }
}

export function createMessage(type: 'send' | 'receive', content: string): Message {
  const now = new Date()
  const time = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`
  return {
    id: ++messageId,
    type,
    content,
    time,
  }
}
