import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

type LoginMode = 'login' | 'register' | 'admin';

interface CustomerAccount {
  username: string;
  password: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
  imports: [CommonModule, FormsModule]
})
export class LoginComponent {

  username: string = '';
  password: string = '';
  mode: LoginMode = 'login';
  errorMessage: string = '';

  readonly adminUsername = 'admin';
  private readonly adminPassword = 'admin123';

  setMode(mode: LoginMode) {
    this.mode = mode;
    this.username = '';
    this.password = '';
    this.errorMessage = '';
  }

  login() {
    this.errorMessage = '';
    const username = this.username.trim();

    if (!username || !this.password) {
      this.errorMessage = 'Enter your username and password.';
      return;
    }

    if (this.mode === 'register') {
      this.registerCustomer(username);
      return;
    }

    if (this.mode === 'admin') {
      this.loginAdmin(username);
      return;
    }

    this.loginCustomer(username);
  }

  private registerCustomer(username: string) {
    const customers = this.getCustomers();
    const existingCustomer = customers.find(
      (customer) => customer.username.toLowerCase() === username.toLowerCase()
    );

    if (existingCustomer) {
      this.errorMessage = 'That customer account already exists. Please log in.';
      return;
    }

    customers.push({ username, password: this.password });
    this.saveCustomers(customers);
    this.startSession(username, 'customer');
  }

  private loginCustomer(username: string) {
    const customer = this.getCustomers().find(
      (account) =>
        account.username.toLowerCase() === username.toLowerCase() &&
        account.password === this.password
    );

    if (!customer) {
      this.errorMessage = 'Customer name or password is incorrect.';
      return;
    }

    this.startSession(customer.username, 'customer');
  }

  private loginAdmin(username: string) {
    if (username !== this.adminUsername || this.password !== this.adminPassword) {
      this.errorMessage = 'Admin name or password is incorrect.';
      return;
    }

    this.startSession('Admin', 'admin');
  }

  private startSession(username: string, role: 'customer' | 'admin') {
    if (typeof window === 'undefined') return;

    localStorage.setItem('role', role);
    localStorage.setItem('user', username);

    location.reload();
  }

  private getCustomers(): CustomerAccount[] {
    if (typeof window === 'undefined') return [];

    const storedCustomers = localStorage.getItem('customers');

    if (!storedCustomers) return [];

    try {
      return JSON.parse(storedCustomers) as CustomerAccount[];
    } catch {
      return [];
    }
  }

  private saveCustomers(customers: CustomerAccount[]) {
    if (typeof window === 'undefined') return;

    localStorage.setItem('customers', JSON.stringify(customers));
  }
}
