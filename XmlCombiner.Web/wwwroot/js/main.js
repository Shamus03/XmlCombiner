'use strict';

function sliceSortStringProperty(original, selector) {
    return original.slice().sort((a, b) => selector(a).localeCompare(selector(b)));
}

const FeedGroupRowVue = {
    props: ['feedGroup'],
    template: '#feedGroupRowTemplate',
    data() {
        return {
            loading: false
        };
    },
    created() {
        this.doWithLoading = async (action) => {
            this.loading = true;
            try {
                await action();
            }
            finally {
                this.loading = false;
            }
        }
    },
    methods: {
        confirmDeleteSelf() {
            if (confirm(`Delete feed group "${this.feedGroup.description}"?`)) {
                this.deleteSelf();
            }
        },
        deleteSelf() {
            this.doWithLoading(async () => {
                await axios.delete(`/api/feedgroups/${this.feedGroup.id}`);
                this.$emit('removeFeedGroup', { id: this.feedGroup.id });
            });
        },
        hideSelf() {
            this.doWithLoading(async () => {
                await axios.put(`/api/feedgroups/${this.feedGroup.id}/hide`);
                this.feedGroup.hidden = true;
            });
        },
        unhideSelf() {
            this.doWithLoading(async () => {
                await axios.put(`/api/feedgroups/${this.feedGroup.id}/unhide`);
                this.feedGroup.hidden = false;
            });
        },
        copySelf() {
            this.doWithLoading(async () => {
                let resp = await axios.post(`/api/feedgroups/${this.feedGroup.id}/copy`);
                let copy = resp.data;
                this.$emit('createFeedGroup', { newFeedGroup: copy });
            });
        },
        renameSelf() {
            this.doWithLoading(async () => {
                let newDescription = prompt('Enter the new description for this feed group:', this.feedGroup.description);

                if (!newDescription) {
                    return;
                }

                await axios.patch(`/api/feedgroups/${this.feedGroup.id}`, { description: newDescription });
                this.feedGroup.description = newDescription;
            });
        }
    }
}

const FeedGroupsVue = {
    template: '#feedGroupsTemplate',
    data() {
        return {
            loading: false,
            creating: false,
            feedGroups: null,
            error: null,
            newFeedGroupDescription: '',
            newFeedGroupBaseUrl: ''
        };
    },
    components: {
        'feed-group-row': FeedGroupRowVue
    },
    created() {
        this.fetchData();
    },
    computed: {
        feedGroupsSortedByDescription() {
            return sliceSortStringProperty(this.feedGroups, f => f.description);
        }
    },
    methods: {
        async fetchData() {
            this.error = this.feedGroups = null;
            this.loading = true;

            try {
                let resp = await axios.get('/api/feedgroups');
                this.feedGroups = resp.data;
            }
            catch (e) {
                this.error = JSON.stringify(e);
            }
            finally {
                this.loading = false;
            }
        },
        async removeFeedGroup(id) {
            this.feedGroups = this.feedGroups.filter(f => f.id !== id);
        },
        async createFeedGroup(newFeedGroup) {
            this.creating = true;

            try {
                if (!newFeedGroup) {
                    let resp = await axios.post('/api/feedgroups', {
                        description: this.newFeedGroupDescription,
                        baseUrl: this.newFeedGroupBaseUrl
                    });
                    newFeedGroup = resp.data;
                }

                this.feedGroups.push(newFeedGroup);
            }
            catch (e) {
                if (e.status === 400) {
                    let errs = [];
                    for (let k in e.responseJSON) {
                        errs.push(e.responseJSON[k][0]);
                    }

                    this.error = errs.join(" ");
                }
                else {
                    this.error = JSON.stringify(e);
                }
            }
            finally {
                this.creating = false;
            }
        }
    }
};

const FeedRowVue = {
    props: ['feed'],
    template: '#feedRowTemplate',
    data() {
        return {
            deleting: false
        }
    },
    computed: {
        sortedAdditionalParameters() {
            return sliceSortStringProperty(this.feed.additionalParameters, p => p.parameter);
        }
    },
    methods: {
        confirmDeleteSelf() {
            if (confirm(`Delete feed "${this.feed.name}"?`)) {
                this.deleteSelf();
            }
        },
        async deleteSelf() {
            this.deleting = true;

            try {
                await axios.delete(`api/feeds/${this.feed.id}`);
                this.$emit('deleteFeed', { id: this.feed.id });
            }
            finally {
                this.deleting = false;
            }
        }
    }
}

const FeedGroupVue = {
    props: ['id'],
    template: '#feedGroupTemplate',
    data() {
        return {
            feedGroup: {},
            loaded: false,
            loading: false,
            updatingBaseUrl: false,
            lastBaseUrl: null,
            error: null,
            newFeedName: '',
            newFeedAdditionalParameters: '',
        };
    },
    components: {
        'feed-row': FeedRowVue
    },
    created() {
        this.fetchData();
    },
    computed: {
        dirty() {
            return this.lastBaseUrl && this.lastBaseUrl != this.feedGroup.baseUrl;
        },
        feedsSortedByName() {
            return sliceSortStringProperty(this.feedGroup.feeds, f => f.name);
        }
    },
    methods: {
        async fetchData() {
            this.loading = true;

            try {
                let resp = await axios.get(`/api/feedgroups/${this.id}`);
                this.feedGroup = resp.data;
                this.lastBaseUrl = this.feedGroup.baseUrl;
                this.loaded = true;
            }
            catch (e) {
                this.error = e.toString();
            }
            finally {
                this.loading = false;
            }
        },
        async submitNewFeed() {
            try {
                let resp = await axios.post(`/api/feedgroups/${this.id}/feeds`, {
                    name: this.newFeedName,
                    baseUrl: this.feedGroup.baseUrl,
                    additionalParameters: this.newFeedAdditionalParameters.trim().split(/\s+/).filter(p => p).map(p => { return { parameter: p }; })
                });
                let newFeed = resp.data;

                this.feedGroup.feeds.push(newFeed);
            }
            catch (e) {
                this.error = e.toString();
            }
        },
        async deleteFeed(id) {
            this.feedGroup.feeds = this.feedGroup.feeds.filter(f => f.id !== id);
        },
        async updateBaseUrl() {
            this.updatingBaseUrl = true;

            try {
                await axios.patch(`/api/feedgroups/${this.id}`, { baseUrl: this.feedGroup.baseUrl });
                this.lastBaseUrl = this.feedGroup.baseUrl;
            } finally {
                this.updatingBaseUrl = false;
            }
        }
    }
};

const routes = [
    { path: '/feedgroups', component: FeedGroupsVue },
    { path: '/feedgroup/:id', component: FeedGroupVue, props: true }
];

const router = new VueRouter({
    routes
});

const app = new Vue({
    router
}).$mount('#app');