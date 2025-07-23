/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_APP_API_BASEURL: string;
    readonly VITE_APP_SIGNALR_BASEURL: string;
    readonly VITE_APP_MAX_SEATS: string;
}
  
interface ImportMeta {
    readonly env: ImportMetaEnv
}