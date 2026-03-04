import { Component, OnInit, inject, ViewChild, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
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
import { LucideAngularModule, RefreshCw, Zap, Filter } from 'lucide-angular';
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
    DateInputsModule,
    IconsModule,
    LucideAngularModule,
    ReactiveFormsModule
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

  filterForm = new FormGroup({
    searchText: new FormControl(''),
    startDate: new FormControl<Date | null>(null),
    endDate: new FormControl<Date | null>(null)
  });

  skip = 0;
  pageSize = 10;
  gridView: GridDataResult = { data: [], total: 0 };
  loading = false;

  ngOnInit(): void {
    this.fetchLiveCommits();

    this.filterForm.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.refresh();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  fetchLiveCommits(): void {
    this.loading = true;
    const page = Math.floor(this.skip / this.pageSize) + 1;
    const { searchText, startDate, endDate } = this.filterForm.value;
    
    this.githubClient.getLive(
        searchText || null, 
        startDate || null, 
        endDate || null, 
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
}
