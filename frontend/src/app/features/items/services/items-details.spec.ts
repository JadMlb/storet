import { TestBed } from '@angular/core/testing';

import { ItemsDetails } from './items-details';

describe('ItemsDetails', () => {
  let service: ItemsDetails;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ItemsDetails);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
