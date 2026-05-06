import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockTag } from './stock-tag';

describe('StockTag', () => {
  let component: StockTag;
  let fixture: ComponentFixture<StockTag>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StockTag]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StockTag);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
