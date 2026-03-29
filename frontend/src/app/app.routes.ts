import { Routes } from '@angular/router';
import { Categories } from './pages/categories/categories';
import { CategoryDetails } from './pages/category-details/category-details';
import { Items } from './pages/items/items';
import { ItemDetails } from './pages/item-details/item-details';
import { AppLayout } from './components/app-layout/app-layout';
import { authGuard } from './guards/auth-guard';
import { Login } from './pages/auth/login/login';
import { Signup } from './pages/auth/signup/signup';

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