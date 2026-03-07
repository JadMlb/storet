import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListViewItemToggleExpandButton } from './list-view-item-toggle-expand-button';

describe('CategoryToggleExpandButton', () => {
  let component: ListViewItemToggleExpandButton;
  let fixture: ComponentFixture<ListViewItemToggleExpandButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListViewItemToggleExpandButton]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListViewItemToggleExpandButton);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
