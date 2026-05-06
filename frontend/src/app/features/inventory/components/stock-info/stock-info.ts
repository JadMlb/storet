import { Component, input } from '@angular/core';
import { Stock } from '../../models/Stock';
import { StockTag } from './stock-tag/stock-tag';

@Component ({
  selector: 'stock-info',
  imports: [StockTag],
  templateUrl: './stock-info.html',
  styleUrl: './stock-info.scss',
})
export class StockInfo
{
  stock = input<Stock>();
}