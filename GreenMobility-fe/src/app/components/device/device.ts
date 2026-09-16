import { Component, OnDestroy, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DeviceService } from '../../services/device';
import { Subscription, timer, interval } from 'rxjs';

@Component({
  selector: 'app-device',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './device.html',
  styleUrl: './device.css'
})
export class Device implements OnDestroy, OnInit {
  step: 'home' | 'code' | 'timer' | 'report' | 'operator' = 'home';

  apiKey: string = '';
  operatorApiKey: string = '';
  operatorError: string | null = null;
  deviceCode: string = '';
  batteryLevel: number = 100;
  isVerifying: boolean = false;
  verifyError: string | null = null;

  isTimerRunning: boolean = false;
  isWaitingToStart: boolean = false;
  startCountdown: number = 5;
  timerSeconds: number = 0;
  private timerInterval: any;
  private startInterval: any;

  
  startTime: Date | null = null;
  endTime: Date | null = null;
  backendCost: number = 0;
  backendDuration: string = '';
  reportCountdown: number = 30;
  private reportInterval: any;

  constructor(
    private deviceService: DeviceService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  
  private async getCurrentBatteryLevel(): Promise<number> {
    if (!('getBattery' in navigator)) {
      return 100; 
    }

    try {
      const battery: any = await (navigator as any).getBattery();
      const level = Math.round(battery.level * 100);
      return level;
    } catch (e) {
      console.warn('[Battery] getBattery() fallito:', e);
      return 100;
    }
  }

  ngOnInit() {
    if (typeof localStorage !== 'undefined') {
      this.apiKey = localStorage.getItem('api key') || '';
    }

    this.route.paramMap.subscribe(params => {
      const key = params.get('apikey');
      if (key) {
        this.apiKey = key;
        if (typeof localStorage !== 'undefined') {
          localStorage.setItem('api key', key);
        }
      }
    });

    if (this.router.url.includes('/apikey')) {
      this.goToOperator();
    } else {
      this.step = 'home';
    }

    if (typeof window !== 'undefined') {
      window.addEventListener('touchmove', this.blurActiveElement);
      window.addEventListener('scroll', this.blurActiveElement);
    }
  }

  private blurActiveElement = () => {
    const activeEl = document.activeElement;
    if (activeEl && (activeEl.tagName === 'INPUT' || activeEl.tagName === 'TEXTAREA')) {
      (activeEl as HTMLElement).blur();
    }
  };

  async onAvvia() {
    this.batteryLevel = await this.getCurrentBatteryLevel();
    this.step = 'code';
    this.cdr.detectChanges();
  }

  goToOperator() {
    this.operatorApiKey = '';
    this.operatorError = null;
    this.step = 'operator';
    this.cdr.detectChanges();
  }

  saveOperatorKey() {
    const guidRegex = /^[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}$/;
    if (!guidRegex.test(this.operatorApiKey)) {
      this.operatorError = 'Formato non valido. (Es: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX)';
      return;
    }
    this.apiKey = this.operatorApiKey;
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('api key', this.apiKey);
    }
    this.router.navigate(['/vehicledevice']).then(() => {
      this.step = 'home';
      this.cdr.detectChanges();
    });
  }

  cancelOperator() {
    this.router.navigate(['/vehicledevice']).then(() => {
      this.step = 'home';
      this.cdr.detectChanges();
    });
  }

  ngOnDestroy() {
    this.cleanupIntervals();
    if (typeof window !== 'undefined') {
      window.removeEventListener('touchmove', this.blurActiveElement);
      window.removeEventListener('scroll', this.blurActiveElement);
    }
  }

  private cleanupIntervals() {
    if (this.startInterval) {
      clearInterval(this.startInterval);
      this.startInterval = null;
    }
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
    if (this.reportInterval) {
      clearInterval(this.reportInterval);
      this.reportInterval = null;
    }
  }

  

  onCodeInput(event: any) {
    this.deviceCode = event.target.value.replace(/[^0-9]/g, '').slice(0, 6);
    this.verifyError = null;
  }

  submitCode() {
    if (this.deviceCode.length < 6) {
      this.verifyError = "Il codice deve essere di almeno 6 cifre.";
      return;
    }
    if (!this.apiKey) {
      this.verifyError = "Manca la configurazione dell'API Key del veicolo.";
      return;
    }
    this.verifyError = null;
    this.isVerifying = true;

    this.deviceService.verifyCode(this.apiKey, this.deviceCode).subscribe({
      next: (res) => {
        this.isVerifying = false;

        this.step = 'timer';
        this.isWaitingToStart = true;
        this.timerSeconds = 0;
        this.startTime = null;
        this.endTime = null;
        this.cdr.detectChanges();

        this.startCountdown = 5;
        this.cdr.markForCheck();
        this.cdr.detectChanges();

        this.startInterval = setInterval(() => {
          this.startCountdown--;
          this.cdr.markForCheck();
          this.cdr.detectChanges();

          if (this.startCountdown <= 0) {
            clearInterval(this.startInterval);
            this.startInterval = null;
            this.isWaitingToStart = false;
            this.startTimer();
          }
        }, 1000);
      },
      error: (err) => {
        this.isVerifying = false;
        this.verifyError = typeof err.error === 'string' ? err.error : 'Errore durante la verifica del codice.';
        this.cdr.detectChanges();
      }
    });
  }

  

  get formattedTimer(): string {
    const hours = Math.floor(this.timerSeconds / 3600);
    const minutes = Math.floor((this.timerSeconds % 3600) / 60);
    const seconds = this.timerSeconds % 60;
    return `${this.padZero(hours)}:${this.padZero(minutes)}:${this.padZero(seconds)}`;
  }

  private padZero(num: number): string {
    return num.toString().padStart(2, '0');
  }

  async terminateRide() {
    this.isWaitingToStart = false;
    this.isTimerRunning = false;

    if (this.startInterval) {
      clearInterval(this.startInterval);
      this.startInterval = null;
    }

    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }

    this.endTime = new Date();
    if (!this.startTime) {
      this.startTime = new Date();
    }

    this.batteryLevel = await this.getCurrentBatteryLevel();

    this.deviceService.endRental(this.apiKey, this.batteryLevel).subscribe({
      next: (res) => {
        this.backendCost = res.costo || 0;
        this.backendDuration = res.durata || '';

        this.step = 'report';
        this.startReportCountdown();
        this.cdr.detectChanges();
      },
      error: (err) => {
        alert('Errore durante la chiusura del noleggio: ' + (typeof err.error === 'string' ? err.error : err.message));
        this.resetToStart();
      }
    });
  }

  private startTimer() {
    this.isTimerRunning = true;
    this.startTime = new Date();
    this.cdr.markForCheck();
    this.cdr.detectChanges();

    this.timerInterval = setInterval(() => {
      this.timerSeconds++;
      this.cdr.markForCheck();
      this.cdr.detectChanges();
    }, 1000);
  }

  

  formatTime(date: Date | null): string {
    if (!date) return '--:--';
    const h = date.getHours().toString().padStart(2, '0');
    const m = date.getMinutes().toString().padStart(2, '0');
    return `${h}:${m}`;
  }

  private startReportCountdown() {
    this.reportCountdown = 30;
    this.reportInterval = setInterval(() => {
      this.reportCountdown--;
      this.cdr.markForCheck();

      if (this.reportCountdown <= 0) {
        clearInterval(this.reportInterval);
        this.reportInterval = null;
        this.resetToStart();
      }
    }, 1000);
  }

  
  resetToStart() {
    this.cleanupIntervals();
    this.step = 'home';
    this.deviceCode = '';
    this.timerSeconds = 0;
    this.startTime = null;
    this.endTime = null;
    this.backendCost = 0;
    this.backendDuration = '';
    this.isTimerRunning = false;
    this.isWaitingToStart = false;
    this.cdr.detectChanges();
  }
}