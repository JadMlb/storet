import { TestBed } from '@angular/core/testing';

import { CategoryDetailsService } from './category-details';

describe('CategoryDetails', () => {
  let service: CategoryDetailsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CategoryDetailsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
