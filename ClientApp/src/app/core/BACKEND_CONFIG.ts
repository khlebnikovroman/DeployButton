import { environment } from '../../environments/environment';

export const BACKEND_CONFIG = {
  api: {
    baseUrl: environment.backend.baseUrl,
  },
  signalr: {
    baseUrl: environment.backend.signalrHubUrl
  }
};
