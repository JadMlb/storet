import { TestBed } from '@angular/core/testing';

import { ItemsService } from './items';

describe('Items', () => {
  let service: ItemsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ItemsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
