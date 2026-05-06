import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListViewItemDetails } from './list-view-item-details';

describe('ListViewItemDetails', () => {
  let component: ListViewItemDetails;
  let fixture: ComponentFixture<ListViewItemDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListViewItemDetails]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListViewItemDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
