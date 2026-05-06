export type StockStatus = "empty_accepted" | "empty_not_accepted" | "critical" | "sufficient" | "full";

export const STOCK_STATUS_BADNESS_SCALE = {
  "empty_accepted": 3,
  "empty_not_accepted": 4,
  "critical": 2,
  "sufficient": 1,
  "full": 0
} as const;

export type Stock = {
  itemId: string;
  quantityInStock: number;
  status: StockStatus;
};

export type ItemStock = {
  [itemId: string]: Stock[]
};

export const EMPTY_OR_NULL_STOCK = {
  itemId: "",
  quantityInStock: 0,
  status: "empty_accepted"
} satisfies Stock;

export type ItemStockWithBounds = Stock & {
  minQuantity: number;
  maxQuantity?: number;
};

function getWorseStockStatus (stockA: Stock, stockB: Stock)
{
  const statusABadnessScore = STOCK_STATUS_BADNESS_SCALE[stockA.status];
  const statusBBadnessScore = STOCK_STATUS_BADNESS_SCALE[stockB.status];

  if (statusABadnessScore > statusBBadnessScore)
    return stockA.status;
  return stockB.status;
}

export function calculateTotalStockFromArray (stocks?: Stock[] | null) : Stock
{
  return stocks?.reduce (
    (totalStock, currentItemStock) =>
    {
      totalStock.quantityInStock += currentItemStock.quantityInStock;
      totalStock.status = getWorseStockStatus (totalStock, currentItemStock);

      return totalStock;
    }
  ) ?? EMPTY_OR_NULL_STOCK;
}
