import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListViewItem } from './list-view-item';

describe('Category', () => {
  let component: ListViewItem;
  let fixture: ComponentFixture<ListViewItem>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListViewItem]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListViewItem);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
