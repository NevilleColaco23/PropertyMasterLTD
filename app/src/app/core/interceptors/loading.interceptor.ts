import { HttpContext, HttpContextToken, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { LoaderService } from '../auth/services/loader-service.service';

/**
 * Set on background/polling requests so they don't trigger the blocking
 * full-screen loader. Usage: `http.get(url, { context: silentLoadingContext() })`.
 */
export const SKIP_GLOBAL_LOADER = new HttpContextToken<boolean>(() => false);

export const silentLoadingContext = (): HttpContext =>
  new HttpContext().set(SKIP_GLOBAL_LOADER, true);

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.context.get(SKIP_GLOBAL_LOADER)) {
    return next(req);
  }
  const loaderService = inject(LoaderService);
  loaderService.show();
  return next(req).pipe(
    finalize(() => loaderService.hide())
  );
};
