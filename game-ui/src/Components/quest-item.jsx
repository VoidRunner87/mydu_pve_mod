import {ExpandIcon, SquareIcon} from "./icons";
import {Button, IconButton, TargetButton} from "./buttons";
import styled from "styled-components";

const ExpandItem = styled.div`
    background-color: rgb(27, 48, 56);
    color: rgb(180, 221, 235);
    margin-bottom: 2px;
`;

const Header = styled.div`
    display: flex;
    align-items: center;
    padding: 8px;
    cursor: pointer;

    &:hover {
        background-color: rgb(250, 212, 122);
        color: black;
    }

    &.expanded {
        background-color: rgb(250, 212, 122);
        color: black;
    }
`;

const HeaderText = styled.span`
    padding-left: 4px;
`;

const Contents = styled.div`
    padding: 8px;
    background-color: rgb(13, 24, 28);
`;

const Task = styled.div`
    padding: 8px 0 8px 0;
`;

const ItemContainer = styled.div`
    display: flex;
    align-items: center;
`;

const ItemIcon = styled.div`
    margin-right: 8px;
`;

const ItemText = styled.span`
    margin-right: 8px;
`;

const TaskSubTitle = styled.h3`
    margin: 16px 0 16px 0;
    font-size: 1em;
    font-weight: normal;
`;

const ActionContainer = styled.div`
    display: flex;
    justify-content: start;
`;

const QuestItem = ({title, type, tasks, expanded, onSelect, rewards}) => {

    const tasksRender = tasks
        .map((t, i) => <Task key={i}>
                <ItemContainer>
                    <ItemIcon><SquareIcon/></ItemIcon>
                    <ItemText>{t.title}</ItemText><TargetButton/>
                </ItemContainer>
            </Task>
        );

    const rewardsRender = rewards
        .map((r, i) => <Task key={i}>
                <ItemContainer>
                    <ItemIcon><SquareIcon/></ItemIcon>
                    <ItemText>{r}</ItemText>
                </ItemContainer>
            </Task>
        );

    return (
        <ExpandItem>
            <Header onClick={onSelect} className={expanded ? "expanded" : ""}>
                <ExpandIcon expanded={expanded}/> <HeaderText>{title}</HeaderText>
            </Header>
            <Contents hidden={!expanded}>
                <TaskSubTitle>Objectives:</TaskSubTitle>
                {tasksRender}
                <TaskSubTitle>Rewards:</TaskSubTitle>
                {rewardsRender}
                <br/>
                <ActionContainer>
                    <Button>Accept</Button>
                </ActionContainer>
            </Contents>
        </ExpandItem>
    );
}

export default QuestItem;