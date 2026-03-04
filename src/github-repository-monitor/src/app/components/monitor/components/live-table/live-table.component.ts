import { Component, OnInit, inject, ViewChild, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
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
import { KENDO_DATEINPUTS } from '@progress/kendo-angular-dateinputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DateRangeFilterCellComponent } from '../date-range-filter-cell/date-range-filter-cell.component';
import { IconsModule } from '@progress/kendo-angular-icons';
import { LucideAngularModule, RefreshCw, Zap, Filter } from 'lucide-angular';
import { CompositeFilterDescriptor, FilterDescriptor } from '@progress/kendo-data-query';
import { 
  SVGIcon, 
  searchIcon, 
  fileExcelIcon, 
  filePdfIcon,
  redoIcon as refreshIcon
} from '@progress/kendo-svg-icons';

@Component({
  selector: 'app-live-table',
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
    LucideAngularModule
  ],
  templateUrl: './live-table.component.html',
  styleUrl: './live-table.component.css'
})
export class LiveTableComponent implements OnInit, OnDestroy {
  @ViewChild(DataBindingDirective) dataBinding?: DataBindingDirective;
  private githubClient = inject(GitHubClient);
  private destroy$ = new Subject<void>();

  readonly RefreshIcon = RefreshCw;
  readonly ZapIcon = Zap;
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

  ngOnInit(): void {
    this.fetchLiveCommits();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  fetchLiveCommits(): void {
    this.loading = true;
    const page = Math.floor(this.skip / this.pageSize) + 1;
    
    // Extract filter values from Grid Filter State
    let searchTexts: string[] = [];
    let start: Date | null = null;
    let end: Date | null = null;

    if (this.filterState && this.filterState.filters) {
      this.filterState.filters.forEach(filter => {
        if ('logic' in filter) {
            // It's a CompositeFilterDescriptor (e.g. from our Date Range filter cell)
            filter.filters.forEach(f => {
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
    
    this.githubClient.getLive(
        searchText, 
        start, 
        end, 
        page, 
        this.pageSize
    ).subscribe({
      next: (response) => {
        if (response.success) {
          const data = response.data ?? [];
          this.gridView = {
            data: data,
            total: this.skip + data.length + (data.length === this.pageSize ? 1 : 0)
          };
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error('Failed to fetch live commits', err);
      }
    });
  }

  onPageChange(e: PageChangeEvent): void {
    this.skip = e.skip;
    this.pageSize = e.take;
    this.fetchLiveCommits();
  }

  refresh(): void {
    this.skip = 0;
    if (this.dataBinding) {
      this.dataBinding.skip = 0;
    }
    this.fetchLiveCommits();
  }

  onFilterChange(filter: CompositeFilterDescriptor): void {
    this.filterState = filter;
    this.refresh();
  }
}
