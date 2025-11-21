import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppRoutingModule } from './app-routing.module';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from './shared/shared.module';

@NgModule({
  imports: [
    CommonModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    SharedModule
  ],
  providers: []
})
export class AppModule { }
