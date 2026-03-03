import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GitHubClient } from '../../services/api-client';
import { GitHubCommitDto } from '../../services/api-client';
import { GridModule } from '@progress/kendo-angular-grid';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { LucideAngularModule, RefreshCw, AlertCircle, History, Zap } from 'lucide-angular';

@Component({
  selector: 'app-monitor',
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    ButtonsModule,
    DialogModule,
    LucideAngularModule
  ],
  templateUrl: './monitor.component.html',
  styleUrl: './monitor.component.css'
})
export class MonitorComponent implements OnInit {
  private githubClient = inject(GitHubClient);

  readonly RefreshIcon = RefreshCw;
  readonly AlertIcon = AlertCircle;
  readonly HistoryIcon = History;
  readonly ZapIcon = Zap;

  liveCommits: GitHubCommitDto[] = [];
  scheduledCommits: GitHubCommitDto[] = [];
  lastSyncTime: string | null = null;
  
  loadingLive = false;
  loadingScheduled = false;
  
  errorDialogOpened = false;
  errorMessage = '';

  ngOnInit(): void {
    this.refreshAll();
  }

  refreshAll(): void {
    this.fetchLiveCommits();
    this.fetchScheduledCommits();
  }

  fetchLiveCommits(): void {
    this.loadingLive = true;
    this.githubClient.getLive().subscribe({
      next: (response) => {
        if (response.success) {
          this.liveCommits = response.data ?? [];
        } else {
          this.showError(response.message ?? 'Failed to fetch live commits.');
        }
        this.loadingLive = false;
      },
      error: (err) => {
        this.showError('Failed to fetch live commits. Please ensure the API is running and accessible.');
        this.loadingLive = false;
        console.error(err);
      }
    });
  }

  fetchScheduledCommits(): void {
    this.loadingScheduled = true;
    this.githubClient.getScheduled().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.scheduledCommits = response.data.commits ?? [];
          this.lastSyncTime = response.data.lastSyncTime ? response.data.lastSyncTime.toLocaleString() : null;
        } else {
          this.showError(response.message ?? 'Unknown error');
        }
        this.loadingScheduled = false;
      },
      error: (err) => {
        this.showError('Failed to fetch scheduled commits. Please ensure the API is running and accessible.');
        this.loadingScheduled = false;
        console.error(err);
      }
    });
  }

  showError(message: string): void {
    this.errorMessage = message;
    this.errorDialogOpened = true;
  }

  closeErrorDialog(): void {
    this.errorDialogOpened = false;
  }
}
