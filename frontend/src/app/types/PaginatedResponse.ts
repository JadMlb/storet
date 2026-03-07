export type Pagination<V> = {
  next: V | null;
  previous: V | null;
};

export type PaginatedResponse<T, V> = Pagination<V> & {
  data: T[];
  hasNext: boolean;
  hasPrevious: boolean;
};