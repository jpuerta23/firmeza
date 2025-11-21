import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VentasRoutingModule } from './ventas-routing.module';
import { VentasListComponent } from './ventas-list/ventas-list.component';

@NgModule({
  declarations: [
    VentasListComponent
  ],
  imports: [
    CommonModule,
    VentasRoutingModule
  ]
})
export class VentasModule { }
