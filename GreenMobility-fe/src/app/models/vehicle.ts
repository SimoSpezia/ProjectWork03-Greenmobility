export interface VehicleStatus {
    vehicleStatusId: number;
    status: string;
}

export interface VehicleType {
    vehicleTypeId: number;
    type: string;
}

export interface Vehicle {
    vehicleId: number;
    vehicleTypeId: number;
    vehicleTypeName?: string;
    vehicleStatusId: number;
    vehicleStatusName?: string;
    hubId: number;
    hubName?: string;
    uic: string;
    batteryLevel: number;
    isDeleted: boolean;
}


export interface VehicleCreateDto {
    vehicleTypeId: number;
    hubId: number;
}

export interface VehicleUpdateDto {
    hubId?: number;
    vehicleTypeId?: number;
    vehicleStatusId?: number;
    batteryLevel?: number;
}
