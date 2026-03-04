import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { GridModule } from '@progress/kendo-angular-grid';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { LabelModule } from '@progress/kendo-angular-label';
import { LucideAngularModule, AlertCircle } from 'lucide-angular';
import { LiveTableComponent } from './components/live-table/live-table.component';
import { ScheduledTableComponent } from './components/scheduled-table/scheduled-table.component';

@Component({
  selector: 'app-monitor',
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    ButtonsModule,
    DialogModule,
    InputsModule,
    DateInputsModule,
    LabelModule,
    LucideAngularModule,
    ReactiveFormsModule,
    LiveTableComponent,
    ScheduledTableComponent
  ],
  templateUrl: './monitor.component.html',
  styleUrl: './monitor.component.css'
})
export class MonitorComponent implements OnInit {
  readonly AlertIcon = AlertCircle;

  errorDialogOpened = false;
  errorMessage = '';

  ngOnInit(): void {
  }

  showError(message: string): void {
    this.errorMessage = message;
    this.errorDialogOpened = true;
  }

  closeErrorDialog(): void {
    this.errorDialogOpened = false;
  }
}
