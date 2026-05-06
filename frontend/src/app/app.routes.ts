import { Routes } from '@angular/router';
import { Categories } from './features/categories/components/categories/categories';
import { CategoryDetails } from './features/categories/components/category-details/category-details';
import { Items } from './features/items/components/items/items';
import { ItemDetails } from './features/items/components/item-details/item-details';
import { AppLayout } from './layout/app-layout';
import { authGuard } from './features/auth/guards/auth-guard';
import { Login } from './features/auth/pages/login/login';
import { Signup } from './features/auth/pages/signup/signup';

export const routes: Routes = [
  {
    path: "login",
    component: Login
  },
  {
    path: "signup",
    component: Signup
  },
  {
    path: "",
    component: AppLayout,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
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
    ]
  }
];