import { Routes } from '@angular/router';
import { Categories } from './pages/categories/categories';
import { CategoryDetails } from './components/category-details/category-details';

export const routes: Routes = [
	{
		path: "categories",
		component: Categories,
		children: [
			{
				path: ":id",
				component: CategoryDetails
			}
		]
	}
];