// Company
export interface CompanyResponse {
  id: string
  name: string
  taxIdentification: string
  country: string
  city: string
  phone: string
  email: string
  status: 'Active' | 'Suspended'
  createdAtUtc: string
  updatedAtUtc?: string | null
}

export interface CreateCompanyRequest {
  name: string
  taxIdentification: string
  country: string
  city: string
  phone: string
  email: string
}

export interface UpdateCompanyProfileRequest {
  name: string
  taxIdentification: string
  country: string
  city: string
  phone: string
  email: string
}

export interface CreateCompanyAdminRequest {
  firstName: string
  lastName: string
  email: string
  password: string
}

export interface CompanyAdminUser {
  id: string
  email: string
  firstName: string
  lastName: string
  companyId?: string | null
  roles: string[]
}

// Client
export interface ClientResponse {
  id: string
  companyId: string
  name: string
  clientCode: string
  taxIdentification?: string | null
  taxId?: string | null
  contactName?: string | null
  contactPerson?: string | null
  email?: string | null
  phone?: string | null
  address?: {
    street?: string
    city?: string
    state?: string
    zipCode?: string
    country?: string
  } | string | null
  status: 'Active' | 'Inactive' | string
  createdAtUtc: string
  updatedAtUtc?: string | null
}

// Employee
export interface EmployeeResponse {
  id: string
  companyId: string
  employeeCode: string
  firstName: string
  lastName: string
  fullName: string
  identityDocument: string
  phone: string
  email: string
  hireDate: string
  hiredOn?: string
  status: 'Active' | 'Suspended' | 'Retired' | string
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

export interface CreatePlannedTripRequest {
  tripNumber: string
  serviceDate: string
  origin: {
    address: string
    latitude?: number
    longitude?: number
  }
  destination: {
    address: string
    latitude?: number
    longitude?: number
  }
  clientId?: string | null
  routeId?: string | null
  agreedAmount?: number | null
  currency?: string | null
}

export interface RegisterCompanyVehicleRequest {
  licensePlate: string
  make: string
  model: string
  manufactureYear: number
  color?: string | null
  type: number | string
  passengerCapacity?: number | null
}

export interface CreateRouteRequest {
  routeCode: string
  name: string
  origin: {
    address: string
    latitude?: number
    longitude?: number
  }
  destination: {
    address: string
    latitude?: number
    longitude?: number
  }
  clientId?: string | null
  instructions?: string | null
  estimatedDurationMinutes?: number | null
  referenceAmount?: number | null
  referenceCurrency?: string | null
}

export interface CreateRouteScheduleRequest {
  routeId: string
  shift: number | string
  startTime: string
  days: number[]
  effectiveFrom: string
  endTime?: string | null
  effectiveUntil?: string | null
  defaultAmount?: number | null
  defaultCurrency?: string | null
}

export interface CreateEmployeeRequest {
  employeeCode: string
  firstName: string
  lastName: string
  identityDocument: string
  phone: string
  email: string
  hireDate: string
}

export interface UpdateEmployeeRequest {
  employeeCode: string
  firstName: string
  lastName: string
  identityDocument: string
  phone: string
  email: string
  hireDate: string
}

export interface CreateClientRequest {
  clientCode: string
  name: string
  taxIdentification?: string | null
  contactName?: string | null
  phone?: string | null
  email?: string | null
}

export interface RouteScheduleResponse {
  id: string
  companyId: string
  routeId: string
  routeName?: string
  shift: string
  startTime: string
  endTime?: string | null
  effectiveFrom: string
  effectiveUntil?: string | null
  days: string[] | number[]
  status: 'Active' | 'Inactive'
  currentAssignment?: {
    employeeId: string
    employeeName?: string
    vehicleId?: string | null
    licensePlate?: string
    assignedFrom: string
  } | null
  createdAtUtc: string
}

export interface PaymentPeriodResponse {
  id: string
  companyId: string
  periodNumber: string
  startDate: string
  endDate: string
  status: 'Open' | 'Closed' | 'Settled'
  totalAmount: number
  currency: string
  createdAtUtc: string
}

export interface PaymentReportResponse {
  id: string
  companyId: string
  employeeId: string
  employeeName: string
  periodId: string
  tripsCount: number
  grossAmount: number
  deductions: number
  netAmount: number
  currency: string
  status: 'Pending' | 'Paid'
  generatedAtUtc: string
}

export interface TripResponse {
  id: string
  companyId: string
  tripNumber: string
  source: 'Administrator' | 'RouteSchedule' | 'Employee'
  status: 'Planned' | 'PendingApproval' | 'Assigned' | 'InProgress' | 'Completed' | 'Cancelled' | 'Rejected'
  clientId?: string | null
  clientName?: string | null
  routeId?: string | null
  routeName?: string | null
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
    employeeName?: string
    vehicleId?: string | null
    licensePlate?: string
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

