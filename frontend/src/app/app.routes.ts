import { Routes } from '@angular/router';
import { Categories } from './pages/categories/categories';
import { CategoryDetails } from './pages/category-details/category-details';

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
	}
];