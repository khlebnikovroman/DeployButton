export interface DeviceState {
  isConnected: boolean;
  portName: string | null;
  baudRate: number | null;
  availablePorts: string[];
}
