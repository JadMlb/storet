import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockInfo } from './stock-info';

describe('StockInfo', () => {
  let component: StockInfo;
  let fixture: ComponentFixture<StockInfo>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StockInfo]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StockInfo);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
