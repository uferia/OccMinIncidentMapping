import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Incident {
  id?: string;
  latitude: number;
  longitude: number;
  hazard: string;
  status: string;
  description: string;
  timestamp: string;
}

@Injectable({ providedIn: 'root' })
export class IncidentService {
  private apiUrl = '/api/Incidents';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Incident[]> {
    return this.http.get<Incident[]>(this.apiUrl);
  }

  getById(id: string): Observable<Incident> {
    return this.http.get<Incident>(`${this.apiUrl}/${id}`);
  }

  create(incident: Incident): Observable<Incident> {
    return this.http.post<Incident>(this.apiUrl, incident);
  }

  update(id: string, incident: Incident): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, incident);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}