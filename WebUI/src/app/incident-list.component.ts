import { Component, OnInit } from '@angular/core';
import { IncidentService, Incident } from './incident.service';

@Component({
  selector: 'app-incident-list',
  template: `
  <p>Incidents loaded: {{ incidents.length }}</p>
    <h2>Incidents</h2>
    <form (ngSubmit)="addIncident()" #incidentForm="ngForm">
      <input [(ngModel)]="newIncident.latitude" name="latitude" placeholder="Latitude" required>
      <input [(ngModel)]="newIncident.longitude" name="longitude" placeholder="Longitude" required>
      <input [(ngModel)]="newIncident.hazard" name="hazard" placeholder="Hazard" required>
      <input [(ngModel)]="newIncident.status" name="status" placeholder="Status" required>
      <input [(ngModel)]="newIncident.description" name="description" placeholder="Description" required>
      <button type="submit">Add</button>
    </form>
    <ul>
      <li *ngFor="let incident of incidents">
        <span>{{ incident.hazard }} - {{ incident.status }} - {{ incident.description }}</span>
        <button (click)="editIncident(incident)">Edit</button>
        <button (click)="deleteIncident(incident)">Delete</button>
      </li>
    </ul>
    <div *ngIf="editingIncident">
      <h3>Edit Incident</h3>
      <form (ngSubmit)="updateIncident()" #editForm="ngForm">
        <input [(ngModel)]="editingIncident.latitude" name="editLatitude" required>
        <input [(ngModel)]="editingIncident.longitude" name="editLongitude" required>
        <input [(ngModel)]="editingIncident.hazard" name="editHazard" required>
        <input [(ngModel)]="editingIncident.status" name="editStatus" required>
        <input [(ngModel)]="editingIncident.description" name="editDescription" required>
        <button type="submit">Update</button>
        <button type="button" (click)="cancelEdit()">Cancel</button>
      </form>
    </div>
  `
})
export class IncidentListComponent implements OnInit {
  incidents: Incident[] = [];
  newIncident: Incident = { latitude: 0, longitude: 0, hazard: '', status: '', description: '', timestamp: new Date().toISOString() };
  editingIncident: Incident | null = null;

  constructor(private incidentService: IncidentService) {}

  ngOnInit() {
    this.loadIncidents();
  }

  loadIncidents() {
    this.incidentService.getAll().subscribe(data => this.incidents = data);
  }

  addIncident() {
    this.incidentService.create(this.newIncident).subscribe(() => {
      this.newIncident = { latitude: 0, longitude: 0, hazard: '', status: '', description: '', timestamp: new Date().toISOString() };
      this.loadIncidents();
    });
  }

  editIncident(incident: Incident) {
    this.editingIncident = { ...incident };
  }

  updateIncident() {
    if (this.editingIncident && this.editingIncident.id) {
      this.incidentService.update(this.editingIncident.id, this.editingIncident).subscribe(() => {
        this.editingIncident = null;
        this.loadIncidents();
      });
    }
  }

  deleteIncident(incident: Incident) {
    if (incident.id) {
      this.incidentService.delete(incident.id).subscribe(() => this.loadIncidents());
    }
  }

  cancelEdit() {
    this.editingIncident = null;
  }
}