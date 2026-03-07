import { Routes } from '@angular/router';
import { Categories } from './pages/categories/categories';
import { CategoryDetails } from './pages/category-details/category-details';
import { Items } from './pages/items/items';
import { ItemDetails } from './pages/item-details/item-details';

export const routes: Routes = [
	{
		path: "categories",
		component: Categories,
		children: [
			{
				path: ":id",
				component: CategoryDetails
			},
			{
				path: "new",
				component: CategoryDetails
			}
		]
	},
	{
		path: "items",
		component: Items,
		children: [
			{
				path: ":id",
				component: ItemDetails
			},
			{
				path: "new",
				component: ItemDetails
			}
		]
	},
];