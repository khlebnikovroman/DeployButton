export interface AppSettings {
  serialPort: SerialPortConfig;
  teamCity: TeamCityConfig;
}

export interface SerialPortConfig {
  portName: string;
  baudRate: number;
}

export interface TeamCityConfig {
  baseUrl: string;
  buildConfigurationId: string;
  username: string;
  password: string;
}
