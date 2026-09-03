// Company
export interface CompanyResponse {
  id: string
  name: string
  taxId: string
  email: string
  phone: string
  address: {
    street: string
    city: string
    state: string
    zipCode: string
    country: string
  }
  status: 'Active' | 'Suspended'
  createdAtUtc: string
  updatedAtUtc?: string | null
}

// Client
export interface ClientResponse {
  id: string
  companyId: string
  name: string
  clientCode: string
  taxId: string
  email: string
  phone: string
  address: {
    street: string
    city: string
    state: string
    zipCode: string
    country: string
  }
  contactPerson: string
  status: 'Active' | 'Inactive'
  createdAtUtc: string
  updatedAtUtc?: string | null
}

// Employee
export interface EmployeeResponse {
  id: string
  companyId: string
  employeeCode: string
  fullName: {
    firstName: string
    lastName: string
  }
  identityDocument: string
  phone: string
  email: string
  hiredOn: string
  status: 'Active' | 'Suspended' | 'Retired'
  userId?: string | null
  createdAtUtc: string
  updatedAtUtc?: string | null
}

// Vehicle
export interface VehicleDocumentResponse {
  id: string
  vehicleId: string
  companyId: string
  documentType: string
  documentNumber: string
  issuedOn: string
  expiresOn: string
  fileStorageKey?: string | null
  notes?: string | null
  createdAtUtc: string
}

export interface VehicleResponse {
  id: string
  companyId: string
  ownershipType: 'CompanyOwned' | 'EmployeeOwned'
  ownerEmployeeId?: string | null
  approvalStatus: 'Approved' | 'PendingApproval' | 'Rejected'
  operationalStatus: 'Operational' | 'Maintenance' | 'Retired'
  make: string
  model: string
  year: number
  color: string
  licensePlate: string
  capacity: number
  documents: VehicleDocumentResponse[]
  createdAtUtc: string
  updatedAtUtc?: string | null
}

// Route
export interface RouteStopResponse {
  id: string
  routeId: string
  companyId: string
  sequence: number
  location: {
    address: string
    latitude: number
    longitude: number
  }
  estimatedDurationMinutes: number
}

export interface RouteResponse {
  id: string
  companyId: string
  name: string
  routeCode: string
  origin: {
    address: string
    latitude: number
    longitude: number
  }
  destination: {
    address: string
    latitude: number
    longitude: number
  }
  estimatedDurationMinutes: number
  defaultBaseFare: number
  currency: string
  status: 'Active' | 'Inactive'
  stops: RouteStopResponse[]
  createdAtUtc: string
  updatedAtUtc?: string | null
}

// Trip
export interface TripIncidentResponse {
  id: string
  tripId: string
  companyId: string
  reportedByEmployeeId: string
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  description: string
  incidentAtUtc: string
  createdAtUtc: string
}

export interface TripResponse {
  id: string
  companyId: string
  tripNumber: string
  source: 'Administrator' | 'RouteSchedule' | 'Employee'
  status: 'Planned' | 'PendingApproval' | 'Assigned' | 'InProgress' | 'Completed' | 'Cancelled' | 'Rejected'
  clientId?: string | null
  routeId?: string | null
  serviceDate: string
  origin: {
    address: string
    latitude: number
    longitude: number
  }
  destination: {
    address: string
    latitude: number
    longitude: number
  }
  agreedAmount: number
  finalAmount?: number | null
  currency: string
  currentAssignment?: {
    employeeId: string
    vehicleId?: string | null
    assignedAtUtc: string
  } | null
  incidents: TripIncidentResponse[]
  createdAtUtc: string
  updatedAtUtc?: string | null
}

// Notification
export interface NotificationResponse {
  id: string
  companyId: string
  recipientUserId: string
  recipientEmployeeId?: string | null
  type: string
  title: string
  message: string
  relatedEntityType?: string | null
  relatedEntityId?: string | null
  status: 'Unread' | 'Read' | 'Archived'
  readAtUtc?: string | null
  archivedAtUtc?: string | null
  createdAtUtc: string
}
