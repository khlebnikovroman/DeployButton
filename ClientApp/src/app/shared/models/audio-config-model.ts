export enum ButtonSoundEventType {
  BuildQueued = 'BuildQueued',
  BuildNotQueued = 'BuildNotQueued',
  BuildSucceeded = 'BuildSucceeded',
  BuildFailed = 'BuildFailed'
}

export interface AudioConfig {
  volume: number;
  sounds: { [key in ButtonSoundEventType]: string };
}