import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AppComponent } from './app/app.component';
import { appRoutes } from './app/app-routing.module';
import { tokenInterceptor } from './app/core/interceptors/token.interceptor';

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(appRoutes),
    provideHttpClient(withInterceptors([tokenInterceptor]))
  ]
})
  .catch((err: unknown) => console.error(err));
