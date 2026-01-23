import { Injectable } from '@angular/core';
@Injectable({
  providedIn: 'root' // This makes the class and its IP config available app-wide
})

export class AppConfig {
    private _config: { [key: string]: string };
    constructor() {
        this._config = { 
            PathAPI: 'https://localhost:44346/'
        };
    }
    get setting(): { [key: string]: string } {
        return this._config;
    }
    get(key: string) {
        return this._config[key];
    }
}