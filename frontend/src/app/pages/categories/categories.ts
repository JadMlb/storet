import { Component, inject, OnInit } from '@angular/core';
import { Category } from '../../components/category/category';
import { CategoriesService } from '../../services/categories';
import { Suspense } from '../../components/suspense/suspense';

@Component ({
  selector: 'categories',
  imports: [Category, Suspense],
  templateUrl: './categories.html',
  styleUrl: './categories.scss',
})
export class Categories implements OnInit
{
  categoriesStore = inject (CategoriesService);
  state = this.categoriesStore.getState();

  ngOnInit ()
  {
    this.categoriesStore.get();
  }
}
