import { Routes } from '@angular/router';
import { Categories } from './features/categories/components/categories/categories';
import { Items } from './features/items/components/items/items';
import { AppLayout } from './layout/app-layout';
import { authGuard } from './features/auth/guards/auth-guard';
import { Login } from './features/auth/pages/login/login';
import { Signup } from './features/auth/pages/signup/signup';
import { Empty } from './shared/components/empty/empty';

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
    				component: Empty
     			},
     			{
    				path: "new",
    				component: Empty
     			}
    		]
     	},
     	{
    		path: "items",
    		component: Items,
    		children: [
     			{
    				path: ":id",
    				component: Empty
     			},
     			{
    				path: "new",
    				component: Empty
     			}
    		]
     	},
    ]
  }
];