import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { GitHubClient } from '../../../../services/api-client';
import { 
  GridModule, 
  PageChangeEvent, 
  GridDataResult, 
  DataBindingDirective,
  PDFModule,
  ExcelModule
} from '@progress/kendo-angular-grid';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { IconsModule } from '@progress/kendo-angular-icons';
import { LucideAngularModule, RefreshCw, History, Filter } from 'lucide-angular';
import { 
  SVGIcon, 
  searchIcon, 
  fileExcelIcon, 
  filePdfIcon,
  redoIcon as refreshIcon
} from '@progress/kendo-svg-icons';

@Component({
  selector: 'app-scheduled-table',
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    PDFModule,
    ExcelModule,
    ButtonsModule,
    InputsModule,
    DateInputsModule,
    IconsModule,
    LucideAngularModule,
    ReactiveFormsModule
  ],
  templateUrl: './scheduled-table.component.html',
  styleUrl: './scheduled-table.component.css'
})
export class ScheduledTableComponent implements OnInit {
  @ViewChild(DataBindingDirective) dataBinding?: DataBindingDirective;
  private githubClient = inject(GitHubClient);

  readonly RefreshIcon = RefreshCw;
  readonly HistoryIcon = History;
  readonly FilterIcon = Filter;

  public searchSVG: SVGIcon = searchIcon;
  public excelSVG: SVGIcon = fileExcelIcon;
  public pdfSVG: SVGIcon = filePdfIcon;
  public refreshSVG: SVGIcon = refreshIcon;

  filterForm = new FormGroup({
    searchText: new FormControl(''),
    startDate: new FormControl<Date | null>(null),
    endDate: new FormControl<Date | null>(null)
  });

  skip = 0;
  pageSize = 10;
  gridView: GridDataResult = { data: [], total: 0 };
  loading = false;
  lastSyncTime: string | null = null;

  ngOnInit(): void {
    this.fetchScheduledCommits();
  }

  fetchScheduledCommits(): void {
    this.loading = true;
    const page = Math.floor(this.skip / this.pageSize) + 1;
    const { searchText, startDate, endDate } = this.filterForm.value;
    
    this.githubClient.getScheduled(
        searchText || null, 
        startDate || null, 
        endDate || null, 
        page, 
        this.pageSize
    ).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.gridView = {
            data: response.data.commits ?? [],
            total: response.data.totalCount ?? 0
          };
          this.lastSyncTime = response.data.lastSyncTime ? response.data.lastSyncTime.toLocaleString() : null;
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error('Failed to fetch scheduled commits', err);
      }
    });
  }

  onPageChange(e: PageChangeEvent): void {
    this.skip = e.skip;
    this.pageSize = e.take;
    this.fetchScheduledCommits();
  }

  refresh(): void {
    this.skip = 0;
    if (this.dataBinding) {
      this.dataBinding.skip = 0;
    }
    this.fetchScheduledCommits();
  }
}
