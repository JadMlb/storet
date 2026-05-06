import { TestBed } from '@angular/core/testing';

import { InventoryDetailsService } from './inventory-details-service';

describe('InventoryDetailsService', () => {
  let service: InventoryDetailsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(InventoryDetailsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
