import {useEffect, useState} from "react";
import {CloseButton, Container, Header, Panel, PanelBody, Title, Tab} from "./panel";
import styled from "styled-components";
import QuestItem from "./quest-item";
import {IconButton} from "./buttons";
import {RefreshIcon, TargetIcon} from "./icons";

const CategoryPanel = styled.div`
    background-color: rgb(13, 24, 28);
    min-width: 200px;
    display: flex;
    align-items: center;
    background-repeat: no-repeat;
    background-position: -200px 0;
    background-position-y: bottom;
    background-blend-mode: multiply;
`;

const TabContainer = styled.div`
    width: 100%;
    padding: 0 8px;

    * {
        margin-bottom: 2px;
    }
`;

const ContentPanel = styled.div`
    padding: 16px;
    flex-grow: 1;
`;

const EmptyPanel = styled.div`
    align-content: center;
    color: rgb(180, 221, 235);
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100%;
    flex-direction: column;
`;

const PlayerQuestList = (props) => {

    const [jobs, setJobs] = useState([]);
    const [expandedMap, setExpandedMap] = useState({});
    const [error, setError] = useState("");
    const [abandonedMap, setAbandonedMap] = useState({});

    useEffect(() => {

        if (!window.global_resources) {
            return;
        }

        fetchData();

    }, []);

    const fetchData = () => {

        try {
            if (!window.global_resources) {
                setError("Null Global Resources");
                return;
            }

            let url = window.global_resources["player-quests"];

            fetch(url)
                .then(res => {
                    if (!res.ok) {
                        setError(`Failed HTTP Status: ${res.status}`);
                        return [];
                    }

                    return res.json();
                }, err => {
                    window.modApi.cb(`Caught Error ${err}`);
                })
                .then(data => {

                    if (!data) {
                        window.modApi.cb(`Invalid Data`);
                        return;
                    }

                    setJobs(data.jobs);
                }, err => {
                    window.modApi.cb(`Caught Error 2 ${err}`);
                });
        } catch (e) {
            setError(`Thrown Error ${e}`);
            window.modApi.cb(`Thrown Error ${e}`);
        }
    };

    const handleClose = () => {
        document.getElementById("root").remove();
    };

    const handleSelect = (item) => {
        if (expandedMap[item.id] === true) {
            setExpandedMap({});
        } else {
            setExpandedMap({[item.id]: true});
        }
    };

    const handleAbandon = (index, questId) => {
        abandonedMap[questId] = true;
        setAbandonedMap(abandonedMap);
        jobs.splice(index, 1);
        setJobs([].concat(jobs));

        handleRefresh();
    };

    const questItems = jobs.map((item, index) =>
        <QuestItem key={index}
                   onSelect={() => handleSelect(item)}
                   questId={item.id}
                   rewards={item.rewards}
                   title={item.title}
                   tasks={item.tasks}
                   type={item.type}
                   safe={item.safe}
                   accepted={true}
                   canAbandon={true}
                   onAbandon={() => handleAbandon(index, item.id)}
                   abandoned={abandonedMap[item.id]}
                   expanded={expandedMap[item.id]}/>
    );

    const handleRefresh = () => {
        window.modApi.refreshPlayerQuestList();

        setTimeout(() => {
            fetchData();
        }, 1000);
    };

    const playerInfo = window.modApi.getPlayerInfo();
    const playerAvatarUrl = window.modApi.imageUrl(playerInfo.skinIcon);

    return (
        <Container>
            <Panel>
                <Header>
                    <Title>{playerInfo.playerName} board</Title>
                    <IconButton onClick={handleRefresh}><RefreshIcon/></IconButton>
                    <CloseButton onClick={handleClose}>&times;</CloseButton>
                </Header>
                <PanelBody>
                    <CategoryPanel style={{backgroundImage: `url(${playerAvatarUrl})`}}>
                        <TabContainer>
                            <Tab selected={true}>Missions ({jobs.length})</Tab>
                        </TabContainer>
                    </CategoryPanel>
                    <ContentPanel>
                        <div>{error}</div>
                        {questItems}
                        {questItems.length === 0 ?
                            <EmptyPanel>
                                <p>No active missions. Visit a faction representative to find jobs.</p>
                                <p>You can find them at markets or faction installations.</p>
                            </EmptyPanel>
                            : ""}
                    </ContentPanel>
                </PanelBody>
            </Panel>
        </Container>
    );
}

export default PlayerQuestList;