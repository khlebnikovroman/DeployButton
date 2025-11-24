export interface AppSettings {
  serialPort: SerialPortConfig;
  audio: AudioConfig;
  teamCity: TeamCityConfig;
}

export interface SerialPortConfig {
  portName: string;
  baudRate: number;
}

export interface AudioConfig {
  volume: number;
  deployStart: SoundEventConfig;
  buildSuccess: SoundEventConfig;
  buildFail: SoundEventConfig;
}

export interface SoundEventConfig {
  enabled: boolean;
  soundIds: string[];
  selectedSoundId: string;
}

export interface TeamCityConfig {
  baseUrl: string;
  buildConfigurationId: string;
  username: string;
  password: string;
}
