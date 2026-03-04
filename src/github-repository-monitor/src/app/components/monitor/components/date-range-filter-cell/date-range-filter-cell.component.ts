import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseFilterCellComponent, FilterService } from '@progress/kendo-angular-grid';
import { KENDO_DATEINPUTS } from '@progress/kendo-angular-dateinputs';
import { CompositeFilterDescriptor, FilterDescriptor } from '@progress/kendo-data-query';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-date-range-filter-cell',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, KENDO_DATEINPUTS],
  template: `
    <div class="filter-group date-filter-inline" [formGroup]="filterForm">
      <kendo-daterange>
        <kendo-dateinput
          kendoDateRangeStartInput
          formControlName="start"
          placeholder="Start"
          class="responsive-date"
          [style.width.px]="120"
        ></kendo-dateinput>
        <kendo-dateinput
          kendoDateRangeEndInput
          formControlName="end"
          placeholder="End"
          class="responsive-date"
          [style.width.px]="120"
        ></kendo-dateinput>
        <kendo-daterange-popup [appendTo]="'root'">
          <ng-template kendoDateRangePopupTemplate>
            <kendo-multiviewcalendar 
              kendoDateRangeSelection
            ></kendo-multiviewcalendar>
          </ng-template>
        </kendo-daterange-popup>
      </kendo-daterange>
    </div>
  `,
  styles: [`
    .date-filter-inline {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .responsive-date {
      width: 100% !important;
    }
  `]
})
export class DateRangeFilterCellComponent extends BaseFilterCellComponent {
  @Input() public declare filter: CompositeFilterDescriptor;
  @Input() public field!: string;

  public filterForm = new FormGroup({
    start: new FormControl<Date | null>(null),
    end: new FormControl<Date | null>(null)
  });

  private sub: Subscription;

  constructor(filterService: FilterService) {
    super(filterService);
    this.sub = this.filterForm.valueChanges.subscribe(value => {
      this.applyDateRangeFilter(value.start, value.end);
    });
  }

  public override ngOnDestroy(): void {
    if (this.sub) {
      this.sub.unsubscribe();
    }
    super.ngOnDestroy();
  }

  public get startValue(): Date | null {
    const filter = this.findFilter('gte');
    return filter ? new Date((filter as FilterDescriptor).value) : null;
  }

  public get endValue(): Date | null {
    const filter = this.findFilter('lte');
    return filter ? new Date((filter as FilterDescriptor).value) : null;
  }

  private findFilter(operator: string): FilterDescriptor | CompositeFilterDescriptor | null {
    if (!this.filter || !this.filter.filters) {
      return null;
    }
    return this.filter.filters.find(f => (f as FilterDescriptor).field === this.field && (f as FilterDescriptor).operator === operator) || null;
  }

  protected override applyFilter(filter: CompositeFilterDescriptor): void {
    // Unused, custom implementation handles application.
  }

  private applyDateRangeFilter(start: Date | null | undefined, end: Date | null | undefined): void {
    this.filter = this.removeFilters(this.field);

    if (start || end) {
      const filters: FilterDescriptor[] = [];
      
      if (start) {
        filters.push({
          field: this.field,
          operator: 'gte',
          value: start
        });
      }
      
      if (end) {
        filters.push({
          field: this.field,
          operator: 'lte',
          value: end
        });
      }

      this.filter.filters.push({
        logic: 'and',
        filters: filters
      });
    }

    this.filterService.filter(this.filter);
  }

  private removeFilters(field: string): CompositeFilterDescriptor {
    if (!this.filter || !this.filter.filters) {
      return { logic: 'and', filters: [] };
    }

    const filters = this.filter.filters.filter(f => {
      const isDateFilter = (f as CompositeFilterDescriptor).filters?.some(cf => (cf as FilterDescriptor).field === field);
      return !isDateFilter && (f as FilterDescriptor).field !== field;
    });

    return { logic: 'and', filters: filters };
  }
}
