import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GitHubClient } from '../../../../services/api-client';
import {
  GridModule,
  PageChangeEvent,
  GridDataResult,
  DataBindingDirective,
  PDFModule,
  ExcelModule,
  PagerSettings,
} from '@progress/kendo-angular-grid';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { KENDO_DATEINPUTS } from '@progress/kendo-angular-dateinputs';
import { DateRangeFilterCellComponent } from '../date-range-filter-cell/date-range-filter-cell.component';
import { IconsModule } from '@progress/kendo-angular-icons';
import { LucideAngularModule, RefreshCw, History, Filter } from 'lucide-angular';
import { CompositeFilterDescriptor, FilterDescriptor } from '@progress/kendo-data-query';
import {
  SVGIcon,
  searchIcon,
  fileExcelIcon,
  filePdfIcon,
  redoIcon as refreshIcon,
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
    KENDO_DATEINPUTS,
    DateRangeFilterCellComponent,
    IconsModule,
    LucideAngularModule,
  ],
  templateUrl: './scheduled-table.component.html',
  styleUrl: './scheduled-table.component.css',
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

  public filterState: CompositeFilterDescriptor = { logic: 'and', filters: [] };

  skip = 0;
  pageSize = 10;
  gridView: GridDataResult = { data: [], total: 0 };
  loading = false;
  lastSyncTime: string | null = null;

  public pagerSettings: PagerSettings = {
    buttonCount: 5,
    info: true,
    type: 'numeric',
    pageSizes: [5, 10, 20, 50],
    previousNext: true,
  };

  ngOnInit(): void {
    this.fetchScheduledCommits();
  }

  fetchScheduledCommits(): void {
    this.loading = true;
    const page = Math.floor(this.skip / this.pageSize) + 1;

    // Extract filter values from Grid Filter State
    let searchTexts: string[] = [];
    let start: Date | null = null;
    let end: Date | null = null;

    if (this.filterState && this.filterState.filters) {
      this.filterState.filters.forEach((filter) => {
        if ('logic' in filter) {
          // It's a CompositeFilterDescriptor (e.g. from our Date Range filter cell)
          filter.filters.forEach((f) => {
            if ('operator' in f && 'value' in f) {
              if (f.operator === 'gte') start = f.value;
              if (f.operator === 'lte') end = f.value;
            }
          });
        } else if ('field' in filter && 'value' in filter && filter.value) {
          // It's a single FilterDescriptor (e.g. from Author, Message, SHA inputs)
          searchTexts.push(filter.value);
        }
      });
    }

    const searchText = searchTexts.length > 0 ? searchTexts.join(' ') : null;

    this.githubClient.getScheduled(searchText, start, end, page, this.pageSize).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.gridView = {
            data: response.data.items ?? [],
            total: response.data.totalCount ?? 0,
          };
          this.lastSyncTime = response.data.lastSyncTime
            ? response.data.lastSyncTime.toLocaleString()
            : null;
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error('Failed to fetch scheduled commits', err);
      },
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

  onFilterChange(filter: CompositeFilterDescriptor): void {
    this.filterState = filter;
    this.refresh();
  }
}
