# Module: Transport Management

## Overview
Manages transport routes, vehicles, route stops, and student-to-route assignments. When a student is assigned to a route, their transport fee is automatically synced to their StudentFee record.

---

## Domain Entities

### TransportRoute (`SMS.Domain.Entities.TransportRoute` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| RouteName | string | |
| Description | string | |
| VehicleId | Guid? | FK to Vehicle |
| MonthlyFee | decimal | Auto-added to student fees |
| IsActive | bool | |
| *Nav* | Vehicle, Stops (collection), Assignments (collection) |

### Vehicle (`SMS.Domain.Entities.Vehicle` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| VehicleNumber | string | |
| DriverName | string? | |
| DriverPhone | string? | |
| Capacity | int | |
| IsActive | bool | |

### RouteStop (`SMS.Domain.Entities.RouteStop`)
| Property | Type | Notes |
|----------|------|-------|
| RouteId | Guid | FK |
| StopName | string | |
| SequenceOrder | int | |
| PickupTime | TimeOnly? | |
| DropTime | TimeOnly? | |

### StudentTransportAssignment (`SMS.Domain.Entities.StudentTransportAssignment`)
| Property | Type | Notes |
|----------|------|-------|
| EnrollmentId | Guid | FK |
| RouteId | Guid | FK |
| IsActive | bool | |

---

## API Endpoints

**Controller**: `TransportController` — Route: `api/transport`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/transport/vehicles` | List vehicles |
| POST | `/api/transport/vehicles` | Create vehicle |
| PUT | `/api/transport/vehicles/{id}` | Update vehicle |
| DELETE | `/api/transport/vehicles/{id}` | Delete vehicle |
| GET | `/api/transport/routes` | List routes (with stops) |
| POST | `/api/transport/routes` | Create route (with stops) |
| PUT | `/api/transport/routes/{id}` | Update route |
| DELETE | `/api/transport/routes/{id}` | Delete route |
| POST | `/api/transport/assign` | Assign student to route |
| GET | `/api/transport/assignments` | List all assignments |
| DELETE | `/api/transport/assignments/{id}` | Deactivate assignment |
| GET | `/api/transport/student/{enrollmentId}` | Student transport status |
| POST | `/api/transport/sync-fees` | Sync transport fees to StudentFee records |

---

## CQRS (in `Features/Transport`)

### Commands (each in separate file)
- `AddRouteCommand` — RouteName, Description, VehicleId, MonthlyFee, Stops list
- `UpdateRouteCommand` — Id + same fields
- `AddVehicleCommand` — VehicleNumber, DriverName, DriverPhone, Capacity
- `UpdateVehicleCommand`
- `AssignStudentToTransportCommand` — EnrollmentId, RouteId
- `DeactivateAssignmentCommand` — AssignmentId
- `DeleteTransportCommands` — DeleteRoute, DeleteVehicle
- `SyncTransportFeeCommand` — Syncs route.MonthlyFee → StudentFee.TransportFeeAmount

### Queries (each in separate file)
- `GetRoutesQuery` — List routes with stops and vehicle info
- `GetVehiclesQuery`
- `GetTransportAssignmentsQuery`
- `GetStudentTransportStatusQuery` — EnrollmentId

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/TransportRoute.cs` |
| Entity | `backend/src/SMS.Domain/Entities/Vehicle.cs` |
| Entity | `backend/src/SMS.Domain/Entities/RouteStop.cs` |
| Entity | `backend/src/SMS.Domain/Entities/StudentTransportAssignment.cs` |
| Commands | `backend/src/SMS.Application/Features/Transport/Commands/AddRouteCommand.cs` |
| Commands | `backend/src/SMS.Application/Features/Transport/Commands/AddVehicleCommand.cs` |
| Commands | `backend/src/SMS.Application/Features/Transport/Commands/AssignStudentToTransportCommand.cs` |
| Commands | `backend/src/SMS.Application/Features/Transport/Commands/DeactivateAssignmentCommand.cs` |
| Commands | `backend/src/SMS.Application/Features/Transport/Commands/DeleteTransportCommands.cs` |
| Commands | `backend/src/SMS.Application/Features/Transport/Commands/SyncTransportFeeCommand.cs` |
| Commands | `backend/src/SMS.Application/Features/Transport/Commands/UpdateRouteCommand.cs` |
| Commands | `backend/src/SMS.Application/Features/Transport/Commands/UpdateVehicleCommand.cs` |
| DTOs | `backend/src/SMS.Application/Features/Transport/DTOs/TransportDtos.cs` |
| Handlers | `backend/src/SMS.Application/Features/Transport/Handlers/*.cs` (one per command) |
| Queries | `backend/src/SMS.Application/Features/Transport/Queries/GetRoutesQuery.cs` |
| Queries | `backend/src/SMS.Application/Features/Transport/Queries/GetVehiclesQuery.cs` |
| Queries | `backend/src/SMS.Application/Features/Transport/Queries/GetTransportAssignmentsQuery.cs` |
| Queries | `backend/src/SMS.Application/Features/Transport/Queries/GetStudentTransportStatusQuery.cs` |
| Controller | `backend/src/SMS.API/Controllers/TransportController.cs` |
| Frontend Page | `frontend/src/pages/TransportManagementPage.tsx` |
| Frontend Service | `frontend/src/services/transportService.ts` |
| Frontend API | `frontend/src/services/api.ts` (transportApi section) |

---

## Business Rules
- Route has ordered stops (SequenceOrder)
- Assigning student to route automatically updates StudentFee.TransportFeeAmount
- `SyncTransportFeeCommand` bulk-updates all affected StudentFee records when route MonthlyFee changes
- Transport fee appears as separate line in fee statements/receipts
- Deactivating assignment removes transport fee from future fee calculations
