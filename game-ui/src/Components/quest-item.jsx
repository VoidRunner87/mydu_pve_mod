import {Button} from "./panel";

const QuestItem = ({title, type, tasks}) => {

    const tasksRender = tasks
        .map((t, i) => <div key={i}>{t.title} <Button>Set Location</Button></div>);

    return (
        <div>
            <div>{type} {title}</div>
            <div hidden={true}>{tasksRender}</div>
        </div>
    );
}

export default QuestItem;