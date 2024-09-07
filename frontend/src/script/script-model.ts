export interface ActionModel {
    name?: string;
    type: string;
}

export interface ActionCollection {
    actions: ActionModel[];
}

export interface SpawnAreaModel {
    type: string;
    radius: number;
}

export interface ScriptOverrideModel {
    constructName?: string;
}

export class SphereSpawnAreaModel implements SpawnAreaModel {
    type: string = "sphere";
    radius: number = 200000;
}

export class CompositeActionModel implements ActionModel, ActionCollection {
    name: string = "";
    type: string = "composite";
    actions: ActionModel[] = [];
}

export class SpawnActionModel implements ActionModel {
    type: string = "spawn";
    prefab: string = "";
    minQuantity: number = 1;
    maxQuantity: number = 1;
    override?: ScriptOverrideModel;
}

export class SendMessageActionModel implements ActionModel {
    type: string = "message";
    message: string = "";
}

export class GivePlayerTitleActionModel implements ActionModel {
    type: string = "give-title";
    messsage: string = "";
}

export class DeleteConstructActionModel implements ActionModel {
    type: string = "delete";
}

export class ForEachHandleWithTag implements ActionModel {
    type: string = "for-each-handle-with-tag";
    actions: ActionModel[] = [];
}

export class RandomActionModel implements ActionModel {
    type: string = "random";
    actions: ActionModel[] = [];
}

export type ActionModelFactoryFunc = () => ActionModel;

export class ActionModelFactory {

    create(type: string): ActionModelFactoryFunc | null {

        const map: Record<string, ActionModelFactoryFunc> = {
            'random': () => new RandomActionModel(),
            'for-each-handle-with-tag': () => new ForEachHandleWithTag(),
            'delete': () => new DeleteConstructActionModel(),
            'give-title': () => new GivePlayerTitleActionModel(),
            'message': () => new SendMessageActionModel(),
            'spawn': () => new SpawnActionModel(),
            'composite': () => new CompositeActionModel(),
        };

        if (map[type]) {
            return map[type];
        }

        return null;
    }
}
