import {Vector} from "three/examples/jsm/physics/RapierPhysics";

export interface SectorInstanceItem {
    id: string;
    sector: Vector;
    expiresAt: string;
    forceExpiresAt: string;
    createdAt: string;
    onLoadScript: string;
    onSectorEnterScript: string;
}

const getAll = async (): Promise<SectorInstanceItem[]> => {

    const baseUrl = process.env.REACT_APP_BACKEND_URL;

    const response = await fetch(`${baseUrl}/sector/instance`);

    return response.json();
}

export {
    getAll
}