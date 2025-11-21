import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from './navbar/navbar.component';

@NgModule({
    imports: [
        CommonModule,
        RouterModule,
        NavbarComponent
    ],
    exports: [
        NavbarComponent
    ]
})
export class SharedModule { }
