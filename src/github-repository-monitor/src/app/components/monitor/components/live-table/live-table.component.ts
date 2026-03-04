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
    KENDO_DATEINPUTS,
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

  skip = 0;
  pageSize = 10;
  allCommits: any[] = [];
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
    this.githubClient.getLive().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.allCommits = response.data.items ?? [];
          this.updateGridView();
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        console.error('Failed to fetch live commits', err);
      }
    });
  }

  updateGridView(): void {
    this.gridView = {
      data: this.allCommits.slice(this.skip, this.skip + this.pageSize),
      total: this.allCommits.length
    };
  }

  onPageChange(e: PageChangeEvent): void {
    this.skip = e.skip;
    this.pageSize = e.take;
    this.updateGridView();
  }

  refresh(): void {
    this.skip = 0;
    if (this.dataBinding) {
      this.dataBinding.skip = 0;
    }
    this.fetchLiveCommits();
  }
}
