import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DialogModule } from '@progress/kendo-angular-dialog';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, DialogModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('GitHub Repository Monitor');
}
